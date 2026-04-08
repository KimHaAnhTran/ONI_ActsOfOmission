import os
import json
import requests
from google.oauth2 import service_account

# 1. SETTINGS
DOCUMENT_ID = '1wq6BqfNt_V3_sW4-9rFOCUdqIuJFcYIvsHbIm9lBm2o'
OUTPUT_FILENAME = 'AP Research Paper_FINAL.pdf'

def main():
    try:
        # Auth
        scope = ['https://www.googleapis.com/auth/drive.readonly']
        service_account_info = json.loads(os.environ['GOOGLE_SERVICE_ACCOUNT_JSON'])
        creds = service_account.Credentials.from_service_account_info(service_account_info, scopes=scope)
        
        # Refresh the token
        from google.auth.transport.requests import Request
        creds.refresh(Request())

        # This is the "PC Download" URL structure
        # It bypasses the mobile-style 'Tab 1' cover page entirely
        export_url = f"https://docs.google.com/document/d/{DOCUMENT_ID}/export?format=pdf"

        # Download
        print(f"Downloading from: {export_url}")
        response = requests.get(export_url, headers={'Authorization': f'Bearer {creds.token}'})

        if response.status_code == 200:
            with open(OUTPUT_FILENAME, 'wb') as f:
                f.write(response.content)
            print(f"Success! {OUTPUT_FILENAME} updated. No tab headers found.")
        else:
            print(f"Export failed with status code: {response.status_code}")
            print("Response text:", response.text)
            exit(1)

    except Exception as e:
        print(f"An error occurred: {e}")
        exit(1)

if __name__ == "__main__":
    main()
