import os
import json
import requests
from google.oauth2 import service_account
from googleapiclient.discovery import build

# 1. SETTINGS
DOCUMENT_ID = '1wq6BqfNt_V3_sW4-9rFOCUdqIuJFcYIvsHbIm9lBm2o'
OUTPUT_FILENAME = 'AP Research Paper_FINAL.pdf'

def main():
    try:
        # Auth
        scope = ['https://www.googleapis.com/auth/drive.readonly']
        service_account_info = json.loads(os.environ['GOOGLE_SERVICE_ACCOUNT_JSON'])
        creds = service_account.Credentials.from_service_account_info(service_account_info, scopes=scope)
        
        # Refresh the token to make sure it's active for the download
        from google.auth.transport.requests import Request
        creds.refresh(Request())

        # The specific URL that bypasses the "Tab 1" cover page
        # Note: 'tabId=t.0' is the default ID for the first/only tab in a Google Doc
        export_url = f"https://docs.google.com/feeds/download/documents/export/Export?id={DOCUMENT_ID}&exportFormat=pdf&tabId=t.0"

        # Download the file directly
        response = requests.get(export_url, headers={'Authorization': f'Bearer {creds.token}'})

        if response.status_code == 200:
            with open(OUTPUT_FILENAME, 'wb') as f:
                f.write(response.content)
            print(f"Success! {OUTPUT_FILENAME} updated without tab headers.")
        else:
            print(f"Export failed: {response.status_code}")
            exit(1)

    except Exception as e:
        print(f"An error occurred: {e}")
        exit(1)

if __name__ == "__main__":
    main()
