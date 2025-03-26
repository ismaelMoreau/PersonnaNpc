import os
import re
import subprocess
import requests
import logging
import uuid
from pathlib import Path
import asyncio
import json

from fastapi import FastAPI, HTTPException, File, UploadFile
from fastapi.responses import StreamingResponse, FileResponse
from pydantic import BaseModel
import uvicorn


# Configure logging
logging.basicConfig(level=logging.INFO, 
                    format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

class QuestionRequest(BaseModel):
    question: str

class WorkflowResponse(BaseModel):
    workflow_id: str
    text_response: str
    speech_path: str
    status: str

class WorkflowOrchestrator:
    def __init__(self, 
                 base_dir='/app',
                 deepseek_url='http://localhost:8001/generate',
                 piper_url='http://localhost:8010/generate_speech'):
        """
        Initialize the workflow orchestrator with service URLs and paths
        """
        # Base directories
        self.base_dir = Path(base_dir)
        self.audio2face_dir = self.base_dir / 'Audio2Face-3D-Samples'
        self.deepseek_dir = self.base_dir / 'Deepseek'
        self.piper_dir = self.base_dir / 'Piper'
        
        # Specific paths
        self.audio2face_script = self.audio2face_dir /'scripts' / 'audio2face_3d_microservices_interaction_app' / 'a2f_3d.py'
        self.audio2face_config = self.audio2face_dir / 'scripts'/'audio2face_3d_microservices_interaction_app'/'config'/ 'config_mark.yml'
        
        # URLs
        self.deepseek_url = deepseek_url
        self.piper_url = piper_url
        
        # Output directories
        self.output_dir = self.base_dir / 'workflow_output'
        self.output_dir.mkdir(parents=True, exist_ok=True)
        
        # Store workflow results
        self.workflow_results = {}
    
    def clean_deepseek_response(self, original_response):
        """
        Clean up the DeepSeek response before text-to-speech conversion
        
        :param original_response: Raw response from DeepSeek
        :return: Cleaned and processed response text
        """
        # Remove the 'response':' prefix
        cleaned_response = re.sub(r"^['\"]*response[':\"]*", "", original_response)
        
        # Remove leading numbers and newlines
        cleaned_response = re.sub(r"^[\d\n]+", "", cleaned_response)
        
        # Remove end-of-sentence markers
        cleaned_response = cleaned_response.replace("<｜end▁of▁sentence｜>", "")
        
        # Remove asterisks and content within them (often used for actions/emotions)
        cleaned_response = re.sub(r"\*[^*]*\*", "", cleaned_response)
        
        # Replace multiple whitespaces with a single space
        cleaned_response = re.sub(r'\s+', ' ', cleaned_response)
        
        # Trim surrounding quotes and whitespace
        cleaned_response = cleaned_response.strip().strip('"\'')
        
        # Ensure the first letter is capitalized
        if cleaned_response:
            cleaned_response = cleaned_response[0].upper() + cleaned_response[1:]
        
        # Ensure the response ends with a period if no punctuation
        if cleaned_response and not cleaned_response.endswith(('.', '!', '?')):
            cleaned_response += '.'
        
        return cleaned_response

    def generate_deepseek_response(self, question):
        """Send question to DeepSeek and get processed response"""
        try:
            response = requests.post(self.deepseek_url, json={'question': question})
            response.raise_for_status()
            
            # Extract the raw response
            raw_response = response.json()['response']
            
            # Clean the response
            cleaned_response = self.clean_deepseek_response(raw_response)
            
            logger.info(f"Original DeepSeek Response: {raw_response}")
            logger.info(f"Cleaned DeepSeek Response: {cleaned_response}")
            
            return cleaned_response
        except requests.RequestException as e:
            logger.error(f"DeepSeek generation failed: {e}")
            raise
    
    def generate_speech(self, text, model='en_US-joe-medium'):
        """Generate speech using Piper TTS"""
        try:
            response = requests.post(self.piper_url, 
                                     json={'text': text, 'model': model})
            response.raise_for_status()
            audio_url = response.json()['file_url']
            
            # Download the audio file
            audio_response = requests.get(f"http://localhost:8010{audio_url}")
            audio_path = self.output_dir / f"{uuid.uuid4()}.wav"
            
            with open(audio_path, 'wb') as f:
                f.write(audio_response.content)
            
            return str(audio_path)
        except requests.RequestException as e:
            logger.error(f"Speech generation failed: {e}")
            raise
    
    def run_audio2face(self, audio_path):
        """Run Nvidia Audio2Face to generate animation"""
        try:
            # Run Audio2Face inference
            command = [
                'python3', 
                str(self.audio2face_script), 
                'run_inference', 
                str(audio_path), 
                str(self.audio2face_config), 
                '-u', '0.0.0.0:52000',
            ]
            
            logger.info(f"Running Audio2Face command: {' '.join(command)}")
            
            process = subprocess.run(
                command, 
                capture_output=True, 
                text=True, 
                check=True
            )

            # Log full stdout and stderr
            logger.info(f"Audio2Face stdout:\n{process.stdout}")
            logger.info(f"Audio2Face stderr:\n{process.stderr}")

            # Search for output folders in /app and get the latest one
            output_base_path = Path('/app/')
            output_folders = sorted(output_base_path.glob('output_0000*'), key=lambda p: p.name, reverse=True)

            if output_folders:
                # Get the most recent folder
                output_path = output_folders[0]
                logger.info(f"Audio2Face processing completed. Most recent output folder: {output_path}")
                return str(output_path)
            else:
                logger.error("Could not find any output folders in /app directory.")
                return None

        except subprocess.CalledProcessError as e:
            logger.error(f"Audio2Face process failed with error: {e}")
            logger.error(f"stderr output:\n{e.stderr}")
            return None
        
    async def execute_full_workflow(self, question):
        """
        Execute the complete workflow from question to animation
        
        :param question: Input question to process
        :return: Workflow results with unique ID
        """
        workflow_id = str(uuid.uuid4())
        
        try:
            # 1. Generate DeepSeek response
            deepseek_response = self.generate_deepseek_response(question)
            logger.info(f"DeepSeek Response: {deepseek_response}")
            
            # 2. Generate Speech from response
            speech_path = self.generate_speech(deepseek_response)
            logger.info(f"Speech generated at: {speech_path}")
            
            # 3. Run Audio2Face
            animation_folder = self.run_audio2face(speech_path)
            logger.info(f"Animation generated at: {animation_folder}")
            
            # Store workflow results
            workflow_result = {
                'text_response': deepseek_response,
                'speech_path': speech_path,
                'animation_folder': animation_folder,
                'status': 'completed'
            }
            
            self.workflow_results[workflow_id] = workflow_result
            
            return {
                'workflow_id': workflow_id,
                **workflow_result
            }
        
        except Exception as e:
            error_result = {
                'workflow_id': workflow_id,
                'status': 'failed',
                'error': str(e)
            }
            self.workflow_results[workflow_id] = error_result
            raise HTTPException(status_code=500, detail=str(e))

# Create FastAPI app
app = FastAPI()

# Global orchestrator instance
orchestrator = WorkflowOrchestrator(
    base_dir='/app',
    deepseek_url='http://localhost:8001/generate',
    piper_url='http://localhost:8010/generate_speech'
)

@app.post("/start_workflow")
async def start_workflow(request: QuestionRequest):
    """
    Endpoint to start the workflow from a Unity game
    
    :param request: Question request from the game
    :return: Workflow details
    """
    try:
        result = await orchestrator.execute_full_workflow(request.question)
        return {
            "workflow_id": result['workflow_id'],
            "text_response": result['text_response'],
            "speech_path": result['speech_path'],
            "status": result['status']
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/workflow_output/{workflow_id}")
async def get_workflow_output(workflow_id: str):
    """
    Retrieve the output folder for a specific workflow
    
    :param workflow_id: Unique identifier for the workflow
    :return: Workflow output details
    """
    workflow_result = orchestrator.workflow_results.get(workflow_id)
    
    if not workflow_result:
        raise HTTPException(status_code=404, detail="Workflow not found")
    
    if workflow_result['status'] == 'failed':
        raise HTTPException(status_code=500, detail=workflow_result.get('error', 'Unknown error'))
    
    # Prepare output details
    output_details = {
        'workflow_id': workflow_id,
        'text_response': workflow_result['text_response'],
        'speech_path': workflow_result['speech_path'],
        'animation_folder': workflow_result['animation_folder'],
        'animation_files': os.listdir(workflow_result['animation_folder'])
    }
    
    return output_details
@app.get("/audio_stream/{workflow_id}")
async def get_audio_stream(workflow_id: str):
    """
    Stream audio wave file as bytes to avoid file I/O
    
    :param workflow_id: Unique identifier for the workflow
    :return: Streaming response of audio file
    """
    workflow_result = orchestrator.workflow_results.get(workflow_id)
    
    if not workflow_result:
        raise HTTPException(status_code=404, detail="Workflow not found")
    
    if workflow_result['status'] == 'failed':
        raise HTTPException(status_code=500, detail=workflow_result.get('error', 'Unknown error'))
    
    speech_path = workflow_result['speech_path']
    
    if not os.path.exists(speech_path):
        raise HTTPException(status_code=404, detail="Audio file not found")
    
    # Open the file in binary read mode and stream it
    def iterfile():
        with open(speech_path, mode="rb") as file_like:
            yield from file_like
    
    return StreamingResponse(iterfile(), media_type="audio/wav")

@app.get("/animation_frames/{workflow_id}")
async def get_animation_frames(workflow_id: str):
    """
    Stream animation frames CSV as bytes
    
    :param workflow_id: Unique identifier for the workflow
    :return: Streaming response of CSV file
    """
    workflow_result = orchestrator.workflow_results.get(workflow_id)
    
    if not workflow_result:
        raise HTTPException(status_code=404, detail="Workflow not found")
    
    if workflow_result['status'] == 'failed':
        raise HTTPException(status_code=500, detail=workflow_result.get('error', 'Unknown error'))
    
    animation_folder = workflow_result['animation_folder']
    
    # Hardcode the expected CSV file name
    csv_path = os.path.join(animation_folder, 'animation_frames.csv')
    
    if not os.path.exists(csv_path):
        raise HTTPException(status_code=404, detail="No animation frame CSV found")
    
    # Stream the file directly
    def iterfile():
        with open(csv_path, 'rb') as file_like:
            yield from file_like
    
    # Return StreamingResponse with the appropriate headers
    return StreamingResponse(iterfile(), media_type="text/csv", headers={"Content-Disposition": "attachment; filename=animation_frames.csv"})

def main():
    """
    Run the FastAPI server
    """
    uvicorn.run(app, host="0.0.0.0", port=8020)

if __name__ == "__main__":
    main()