import os
import json
import requests
from google.oauth2 import service_account
from pypdf import PdfReader, PdfWriter  # The splicing library

# 1. SETTINGS
DOCUMENT_ID = '1wq6BqfNt_V3_sW4-9rFOCUdqIuJFcYIvsHbIm9lBm2o'
OUTPUT_FILENAME = 'AP Research Paper_FINAL.pdf'
TEMP_FILENAME = 'temp_raw.pdf'

def main():
    try:
        # Auth
        scope = ['https://www.googleapis.com/auth/drive.readonly']
        service_account_info = json.loads(os.environ['GOOGLE_SERVICE_ACCOUNT_JSON'])
        creds = service_account.Credentials.from_service_account_info(service_account_info, scopes=scope)
        
        # Refresh the token
        from google.auth.transport.requests import Request
        creds.refresh(Request())

        # Download URL
        export_url = f"https://docs.google.com/document/d/{DOCUMENT_ID}/export?format=pdf"
        print(f"Downloading raw PDF...")
        response = requests.get(export_url, headers={'Authorization': f'Bearer {creds.token}'})

        if response.status_code == 200:
            # 1. Save the raw PDF (with the ugly Tab 1 page)
            with open(TEMP_FILENAME, 'wb') as f:
                f.write(response.content)
            
            # 2. Splice it!
            print("Splicing off the cover page...")
            reader = PdfReader(TEMP_FILENAME)
            writer = PdfWriter()

            # Loop through all pages starting at index 1 (skipping index 0)
            for i in range(1, len(reader.pages)):
                writer.add_page(reader.pages[i])

            # 3. Save the clean, final version
            with open(OUTPUT_FILENAME, 'wb') as out_f:
                writer.write(out_f)

            # 4. Clean up the evidence
            if os.path.exists(TEMP_FILENAME):
                os.remove(TEMP_FILENAME)

            print(f"Success! {OUTPUT_FILENAME} updated flawlessly.")
            
        else:
            print(f"Export failed with status code: {response.status_code}")
            exit(1)

    except Exception as e:
        print(f"An error occurred: {e}")
        exit(1)

if __name__ == "__main__":
    main()
