# Use Python as the base image
FROM python:3.10

# Set working directory inside the container
WORKDIR /app

# Install Piper
RUN apt-get update && apt-get install -y espeak-ng && rm -rf /var/lib/apt/lists/*
RUN pip install piper-tts fastapi uvicorn

# Copy server script into the container
COPY server.py .

# Create output directory
RUN mkdir -p /app/output

# Expose API port
EXPOSE 8010

# Run FastAPI when the container starts
CMD ["uvicorn", "server:app", "--host", "0.0.0.0", "--port", "8010"]
