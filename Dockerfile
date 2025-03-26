FROM python:3.12-slim

# Install system dependencies
RUN apt-get update && apt-get install -y \
    python3-venv \
    python3-dev \
    build-essential \
    && rm -rf /var/lib/apt/lists/*

# Set working directory
WORKDIR /app

# Create a virtual environment
RUN python3 -m venv /opt/venv
ENV PATH="/opt/venv/bin:$PATH"


# Copy the workflow script
COPY Ochestrator-server.py /app/Ochestrator-server.py

# Copy the Audio2Face-3D-Samples directory
COPY Audio2Face-3D-Samples /app/Audio2Face-3D-Samples
# Install Python dependencies
COPY requirements.txt .
RUN pip3 install /app/Audio2Face-3D-Samples/proto/sample_wheel/nvidia_ace-1.2.0-py3-none-any.whl
RUN pip install --no-cache-dir -r requirements.txt

# Set environment variables
ENV PYTHONUNBUFFERED=1

# Expose the workflow API port
EXPOSE 8020

# Entrypoint to run the FastAPI server
CMD ["python", "Ochestrator-server.py"]