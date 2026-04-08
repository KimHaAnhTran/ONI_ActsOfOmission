import os
import json
from google.oauth2 import service_account
from googleapiclient.discovery import build
from googleapiclient.http import MediaIoBaseDownload
import io

# 1. SETTINGS
DOCUMENT_ID = '1wq6BqfNt_V3_sW4-9rFOCUdqIuJFcYIvsHbIm9lBm2o'
OUTPUT_FILENAME = 'AP Research Paper_FINAL.pdf'

def main():
    try:
        scope = ['https://www.googleapis.com/auth/drive.readonly']
        service_account_info = json.loads(os.environ['GOOGLE_SERVICE_ACCOUNT_JSON'])
        creds = service_account.Credentials.from_service_account_info(service_account_info, scopes=scope)
        
        # We only need the Drive service for a standard export
        drive_service = build('drive', 'v3', credentials=creds)

        # This exports the entire document to PDF automatically
        request = drive_service.files().export_media(fileId=DOCUMENT_ID, mimeType='application/pdf')
        
        fh = io.FileIO(OUTPUT_FILENAME, 'wb')
        downloader = MediaIoBaseDownload(fh, request)
        done = False
        while done is False:
            status, done = downloader.next_chunk()
            
        print(f"Success! {OUTPUT_FILENAME} updated.")

    except Exception as e:
        print(f"An error occurred: {e}")
        exit(1)

if __name__ == "__main__":
    main()
