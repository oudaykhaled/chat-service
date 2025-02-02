# Chat Service

This guide will help you set up and run the Chat Service, including configuring Firebase and running the necessary infrastructure using Docker Compose.

## Prerequisites

Before running the chat service, ensure you have the following installed:
- [Docker](https://docs.docker.com/get-docker/)
- [Docker Compose](https://docs.docker.com/compose/install/)
- A Firebase account

## Setup

### 1. Register to Firebase
1. Go to [Firebase Console](https://console.firebase.google.com/).
2. Click **Add Project** and follow the setup instructions.
3. Once created, note your **Project ID** from the project settings.

### 2. Create a Firebase Service Account
1. Navigate to **Project Settings > Service Accounts**.
2. Click **Generate new private key**.
3. Download the generated JSON file.
4. Save the file as `firebase-service-account.json` in the root directory of the project.

### 3. Configure API Settings
1. Open the configuration file (`appsettings.json` or relevant config file).
2. Locate the following section:
   ```json
   "Firestore": {
     "ProjectId": "",
     "KeyFilePath": ""
   }
   ```
3. Fill in your Firebase **Project ID** and the path to the service account JSON file:
   ```json
   "Firestore": {
     "ProjectId": "your-project-id",
     "KeyFilePath": "firebase-service-account.json"
   }
   ```
4. Save the file.

### 4. Run the Infrastructure

1. Ensure your `firebase-service-account.json` file is correctly placed.
2. Open a terminal and navigate to the project directory.
3. Run the following command to start the infrastructure:
   ```sh
   docker compose up -d
   ```

This will spin up all required services using Docker Compose.

## Additional Notes
- To stop the services, run:
  ```sh
  docker compose down
  ```
- To check logs, use:
  ```sh
  docker compose logs -f
  ```