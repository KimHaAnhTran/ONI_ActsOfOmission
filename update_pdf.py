import os
import json
from google.oauth2 import service_account
from googleapiclient.discovery import build
from googleapiclient.http import MediaIoBaseDownload

# Authenticate
scope = ['https://www.googleapis.com/auth/drive.readonly']
service_account_info = json.loads(os.environ['GOOGLE_SERVICE_ACCOUNT_JSON'])
creds = service_account.Credentials.from_service_account_info(service_account_info, scopes=scope)
service = build('drive', 'v3', credentials=creds)

# File ID is found in Google Doc URL
FILE_ID = 'YOUR_GOOGLE_DOC_ID_HERE'
request = service.files().export_media(fileId=FILE_ID, mimeType='application/pdf')

with open('document.pdf', 'wb') as f:
    downloader = MediaIoBaseDownload(f, request)
    done = False
    while done is False:
        status, done = downloader.next_chunk()
