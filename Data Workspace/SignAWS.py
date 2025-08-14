import boto3
from botocore.client import Config

session = boto3.session.Session()

client = session.client(
    's3',
    region_name='auto',
    endpoint_url='https://bb2a15ad1a47812a401c380684fe0e9e.r2.cloudflarestorage.com',
    aws_access_key_id='dcaef71606b5cf04e6f166600e15373e',
    aws_secret_access_key='4a5a1a4b5fd2bd56b8fefd3a467278ffbc0c187e7b355a6124d146e277c54141',
    config=Config(signature_version='s3v4')
)

url = client.generate_presigned_url(
    'get_object',
    Params={'Bucket': 'vanguard-card-data', 'Key': 'dataVersion.json'},
    ExpiresIn=7200  # URL valid for 2 hours
)

print("Signed URL:", url)