import json

dataFile = open('allCardsSingleton.json', 'r+')
data = json.loads(urlsFile.read())['cards']

index = 0 # first use
index = len(data.keys()) # later uses
print(index)

dataFile.close()
