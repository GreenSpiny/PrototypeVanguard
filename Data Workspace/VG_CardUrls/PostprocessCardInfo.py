import json

dataFile = open('cardsData.json', 'r+', encoding='raw_unicode_escape')
data = json.loads(dataFile.read().encode('raw_unicode_escape').decode('utf8'))

#cards = data['cards']
cards = data

#index = 0 # first use
index = len(cards.keys()) # subsequent uses

for c in cards.values():

	if 'index' not in c.keys():
		c['index'] = index
		index += 1

	if c['nation'] == '-' or c['nation'] == '':
		c['nation'] = ['Nationless']

	if c['effect'] == '-':
		c['effect'] = ''

	if c['race'] == '-':
		c['race'] = []

	if c['group'] == '-':
		c['group'] = ''

	if 'giftPower' not in c.keys():
		c['giftPower'] = ''

	gift = c['gift']
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

	if 'count' not in c.keys():
		c['count'] = 4

	if 'sixteen' in c['effect']:
		c['count'] = 16
	elif c['gift'] == "Front" or c['gift'] == "Critical" or c['gift'] == "Draw":
		c['count'] = 8
	elif c['gift'] == "Heal":
		c['count'] = 4
	elif c['gift'] == "Over":
		c['count'] = 1

	if isinstance(c['skill'], str):
		skillArray = c['skill'].split(',')
		for i in range(len(skillArray)):
			skillArray[i] = skillArray[i].strip()
		c['skill'] = skillArray
	for i in range(len(c['skill']) - 1, -1, -1):
		if len(c['skill'][i].strip()) == 0:
			c['skill'].pop(i)

	if (isinstance(c['nation'],str)):
		nationArray = c['nation'].split('/')
		c['nation'] = []
		for nation in nationArray:
			c['nation'].append(part.strip())

	if (isinstance(c['race'],str)):
		raceArray = c['race'].split('/')
		c['race'] = []
		for race in raceArray:
			c['race'].append(race.strip())
	for i in range(len(c['race']) - 1, -1, -1):
		if len(c['race'][i].strip()) == 0:
			c['race'].pop(i)

	if 'rotate' not in c.keys():
		c['rotate'] = False

	if 'placeholder' not in c.keys():
		c['placeholder'] = False

	# SPECIAL CASE: COROCORO
	if "CoroCoro" in c['nation'] and len(c['nation']) < 2:
		if 'Oversized Dekka-kun' in c['race']:
			c['nation'].append('Keter Sanctuary')
		elif 'Black Channel' in c['race']:
			c['nation'].append('Brandt Gate')
		elif 'Fate Rewinder' in c['race']:
			c['nation'].append('Dark States')
		elif 'Bebebebe' in c['race']:
			c['nation'].append('Stoicheia')
		elif 'Dangerous Grandpa' in c['race']:
			c['nation'].append('Dragon Empire')
		elif 'Skyride' in c['race']:
			c['nation'].append('Lyrical Monasterio')


dataFile.seek(0)
dataFile.truncate(0)
dataFile.write(json.dumps(data, sort_keys=True, indent=1, ensure_ascii=True))
dataFile.close()
