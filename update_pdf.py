import os
import json
import requests
from google.oauth2 import service_account
from googleapiclient.discovery import build

# 1. SETTINGS - Update these!
DOCUMENT_ID = '1IBRIhuY1owJr_vjxCUqRBrvQjpv7g-7zO6q1hKyOhCY' # Get this from your Doc URL
TARGET_TAB_NAME = 'Paper'
OUTPUT_FILENAME = 'AP Research Paper_FINAL.pdf'

def get_tab_id(docs_service, doc_id, target_name):
    """Finds the ID of a specific tab by its name."""
    doc = docs_service.documents().get(documentId=doc_id).execute()
    tabs = doc.get('tabs', [])
    for tab in tabs:
        if tab.get('tabProperties', {}).get('title') == target_name:
            return tab.get('tabProperties', {}).get('tabId')
    return None

def main():
    try:
        # Authenticate
        scope = [
            'https://www.googleapis.com/auth/drive.readonly',
            'https://www.googleapis.com/auth/documents.readonly'
        ]
        
        if 'GOOGLE_SERVICE_ACCOUNT_JSON' not in os.environ:
            print("Error: GOOGLE_SERVICE_ACCOUNT_JSON secret not found.")
            exit(1)

        service_account_info = json.loads(os.environ['GOOGLE_SERVICE_ACCOUNT_JSON'])
        creds = service_account.Credentials.from_service_account_info(service_account_info, scopes=scope)
        
        # Build services
        docs_service = build('docs', 'v1', credentials=creds)

        # Get the specific Tab ID
        tab_id = get_tab_id(docs_service, DOCUMENT_ID, TARGET_TAB_NAME)
        
        if not tab_id:
            print(f"Error: Could not find a tab named '{TARGET_TAB_NAME}'")
            exit(1)

        # Construct the export URL for that specific tab
        # We use a direct request because the standard library doesn't handle tab-specific exports well yet
        export_url = f"https://docs.google.com/feeds/download/documents/export/Export?id={DOCUMENT_ID}&exportFormat=pdf&tabId={tab_id}"
        
        # Refresh token and fetch
        from google.auth.transport.requests import Request
        creds.refresh(Request())
        
        response = requests.get(export_url, headers={'Authorization': f'Bearer {creds.token}'})

        if response.status_code == 200:
            with open(OUTPUT_FILENAME, 'wb') as f:
                f.write(response.content)
            print(f"Success! {OUTPUT_FILENAME} updated.")
        else:
            print(f"Export failed with status code: {response.status_code}")
            print(response.text)
            exit(1)

    except Exception as e:
        print(f"An error occurred: {e}")
        exit(1)

if __name__ == "__main__":
    main()
