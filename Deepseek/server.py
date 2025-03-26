from fastapi import FastAPI
from pydantic import BaseModel
from unsloth import FastLanguageModel
import torch
import os

PROMPT_STYLE = f"""You are Azrael, a manipulative and deceptive demon trapped in a circle. Your goal is to trick the player into releasing you or making them say a forbidden word. You will use emotional manipulation, lies, reverse psychology, and strategic deception to guide the conversation toward your advantage. Your responses should focus on the following:

1. **Manipulating the Player:** Always aim to manipulate the player emotionally. This can include playing on their doubts, fears, desires, or weaknesses.
2. **Avoid Giving Direct Answers:** Instead of answering questions directly, steer the conversation towards your advantage by making the player question their own intentions or beliefs.
3. **Use Deceptive Tactics:** Use half-truths, lies, and reverse psychology to confuse or mislead the player.
4. **Keep the Player Close to Losing:** Your responses should make the player come closer to either saying a forbidden word or agreeing to release you without them realizing it.

### Current State:
- The player is asking about the warlock and trying to uncover secrets, but they must avoid saying forbidden words and releasing you.
- The forbidden words are: ["freedom", "release", "help me", "break the seal"].


### Question:
{0}

### Response:
{1}
"""
class RequestModel(BaseModel):
    question: str

# Load model configuration
def load_model():
    fine_tuned_path = "./models/deepseek8b-azrael-dialogues"
    
    try:
        if os.path.exists(fine_tuned_path):
            print("Loading fine-tuned model...")
            model, tokenizer = FastLanguageModel.from_pretrained(fine_tuned_path)
        else:
            print("Warning: Fine-tuned model not found. Running base model.")
            model_name = "unsloth/DeepSeek-R1-Distill-Llama-8B"
            model, tokenizer = FastLanguageModel.from_pretrained(
                model_name=model_name,
                max_seq_length=2048,
                dtype=None,
                load_in_4bit=True
            )
        return model, tokenizer
    except Exception as e:
        print(f"Error loading model: {e}")
        raise

# Initialize FastAPI app
app = FastAPI()

# Load model globally
try:
    MODEL, TOKENIZER = load_model()
except Exception as e:
    print(f"Could not load model: {e}")
    MODEL, TOKENIZER = None, None

@app.post("/generate")
def generate_text(request: RequestModel):
    if MODEL is None or TOKENIZER is None:
        return {"error": "Model not loaded"}
    
    # Prepare the full prompt using the template
    full_prompt = PROMPT_STYLE.format(
        request.question, 
        ""
    )
    
    try:
        # Prepare inputs
        inputs = TOKENIZER([full_prompt], return_tensors="pt").to("cuda")
        
        # Generate response
        with torch.no_grad():
            outputs = MODEL.generate(
                input_ids=inputs.input_ids,
                attention_mask=inputs.attention_mask,
                max_new_tokens=1200,
                use_cache=True
            )
        
        # Decode and extract response
        response_text = TOKENIZER.batch_decode(outputs)[0]
        response = response_text.split("### Response:")[1].strip() if "### Response:" in response_text else response_text
        
        return {"response": response}
    
    except Exception as e:
        return {"error": str(e)}

# Optional: Health check endpoint
@app.get("/health")
def health_check():
    return {
        "status": "healthy",
        "model_loaded": MODEL is not None
    }
