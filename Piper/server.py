from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import subprocess
from uuid import uuid4
from pathlib import Path
from fastapi.responses import FileResponse

app = FastAPI()

# Create a Pydantic model for request validation
class SpeechRequest(BaseModel):
    text: str
    model: str = "en_US-joe-medium"

OUTPUT_DIR = Path("/app/output")
OUTPUT_DIR.mkdir(exist_ok=True)

@app.post("/generate_speech")
async def generate_speech(request: SpeechRequest):
    """Generate speech from text using Piper and return a downloadable URL."""
    # Sanitize input
    text = request.text
    model = request.model

    if not text or len(text) > 1000:
        raise HTTPException(status_code=400, detail="Invalid text length")
    
    # Escape special characters
    escaped_text = text.replace('"', '\\"')
    
    output_file = OUTPUT_DIR / f"{uuid4()}.wav"
    
    command = f'echo "{escaped_text}" | piper --model {model} --output_file {output_file}'
    
    try:
        subprocess.run(command, shell=True, check=True)
        
        if not output_file.exists():
            raise HTTPException(status_code=500, detail="Speech generation failed")
        
        return {"status": "success", "file_url": f"/download/{output_file.name}"}
    
    except subprocess.CalledProcessError as e:
        raise HTTPException(status_code=500, detail=f"Speech generation error: {str(e)}")

@app.get("/download/{filename}")
async def download_file(filename: str):
    """Serve generated WAV file."""
    file_path = OUTPUT_DIR / filename
    
    if not file_path.exists():
        raise HTTPException(status_code=404, detail="File not found")
    
    return FileResponse(file_path, media_type="audio/wav", filename=filename)

# # Optional cleanup route to remove old files
# @app.on_event("startup")
# async def cleanup_old_files():
#     """Remove old generated files to prevent disk space issues."""
#     for file in OUTPUT_DIR.glob('*.wav'):
#         if (Path.cwd() / file).stat().st_mtime < (time.time() - 24 * 3600):
#             file.unlink()