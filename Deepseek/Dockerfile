# Use NVIDIA CUDA base image for GPU support
FROM nvidia/cuda:12.4.1-devel-ubuntu22.04

# Set working directory
WORKDIR /app

# Install dependencies
RUN apt update && apt install -y python3 python3-pip git && rm -rf /var/lib/apt/lists/*

# Install Python libraries
RUN pip install --no-cache-dir torch torchvision torchaudio transformers unsloth fastapi uvicorn

# Copy project files
COPY server.py /app/

# Copy fine-tuned model into container
COPY models/deepseek8b-azrael-dialogues /app/models/deepseek8b-azrael-dialogues

# Expose API port
EXPOSE 8001

# Run FastAPI server
CMD ["uvicorn", "server:app", "--host", "0.0.0.0", "--port", "8001"]
