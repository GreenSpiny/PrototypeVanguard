import json

dataFile = open('allCardsSingleton.json', 'r+')
data = json.loads(dataFile.read())
cards = data['cards']

index = 0 # first use
# index = len(cards.keys()) # later uses

for c in cards.values():
	if c['effect'] == '-':
		c['effect'] = ''
	if 'version' not in c.keys():
		c['version'] = 0
	if isinstance(c['skill'], str):
		skillArray = c['skill'].split(',')
		for i in range(len(skillArray)):
			skillArray[i] = skillArray[i].strip()
		c['skill'] = skillArray


dataFile.seek(0)
dataFile.truncate(0)
dataFile.write(json.dumps(data, sort_keys=True, indent=1))
dataFile.close()
