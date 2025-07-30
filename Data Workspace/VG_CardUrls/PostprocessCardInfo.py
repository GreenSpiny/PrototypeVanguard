import json

dataFile = open('allCardsSingleton.json', 'r+')
data = json.loads(dataFile.read())
cards = data['cards']

#index = 0 # first use
index = len(cards.keys()) # subsequent uses

for c in cards.values():
	if isinstance(c['skill'], str):
		skillArray = c['skill'].split(',')
		for i in range(len(skillArray)):
			skillArray[i] = skillArray[i].strip()
		c['skill'] = skillArray

	if 'index' not in c.keys():
		c['index'] = index
		index += 1

	if c['nation'] == '-' or c['nation'] == '':
		c['nation'] = 'Nationless'

	if c['effect'] == '-':
		c['effect'] = ''

	if c['race'] == '-':
		c['race'] = ''

	if c['group'] == '-':
		c['group'] = ''

	if 'giftPower' not in c.keys():
		c['giftPower'] = ""

	gift = c["gift"]
	gift = gift.replace('Trigger', '')
	gift = gift.replace('-', '')
	giftArray = gift.split()
	for i in range(len(giftArray)):
		giftArray[i] = giftArray[i].strip()
	if len(giftArray) == 2:
		c["gift"] = giftArray[0]
		c['giftPower'] = giftArray[1]
	else:
		c["gift"] = gift
	if c["gift"] == '':
		c["giftPower"] = ''

	if 'Twin Drive' in c['skill']:
		c['drive'] = 2
	elif 'Triple Drive' in c['skill']:
		c['drive'] = 3
	else:
		c['drive'] = 1

dataFile.seek(0)
dataFile.truncate(0)
dataFile.write(json.dumps(data, sort_keys=True, indent=1))
dataFile.close()
