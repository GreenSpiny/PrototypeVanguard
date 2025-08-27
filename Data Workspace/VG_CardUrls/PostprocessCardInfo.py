import json

dataFile = open('cardsData.json', 'r+', encoding='raw_unicode_escape')
cards = json.loads(dataFile.read().encode('raw_unicode_escape').decode('utf8'))

# Before running the program, assign jpMode based on if we are importing english cards or JP exclusives.
# When english cards become available, this flag helps us overwrite the JP cards' info without altering their index.
jpMode = False 
index = 0
for c in cards.values():
	if 'index' in c.keys():
		index = max(index, c['index'])

jpCardKeys = []
engCardKeys = []
for key in cards.keys():
	c = cards[key]

	if 'index' not in c.keys():
		index += 1
		c['index'] = index

	if 'jpMode' not in c.keys():
		c['jpMode'] = jpMode;

	if c['jpMode']:
		jpCardKeys.append(key)
	else:
		engCardKeys.append(key)

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

	if "Unit" not in c['type']:
		c['drive'] = 0

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
			c['nation'].append(nation.strip())

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

	# === SPECIAL CASE: COROCORO === #
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

	# === ACTION FLAG SEARCH === #
	lowername = c['name'].lower()
	lowereffect = c['effect'].lower()
	c['actionflags'] = []

	if 'left deity arms' in lowereffect:
		c['actionflags'].append(11) # arm left
	if 'right deity arms' in lowereffect:
		c['actionflags'].append(12) # arm right
	if ' bind ' in lowereffect and ' face down ' in lowereffect:
		c['actionflags'].append(13) # bind FD
	if 'blagndmire' in lowername:
		c['actionflags'].append(14) # bind FD foe
	if ' lock ' in lowereffect:
		c['actionflags'].append(15) # lock
	if 'overDress]' in lowereffect or '[explosivegrowth]' in lowereffect:
		c['actionflags'].append(16) # rideRC
	if 'overDress]' in lowereffect or '[explosivegrowth]' in lowereffect or 'granfia' in lowername:
		c['actionflags'].append(17) # soulRC

dataFile.seek(0)
dataFile.truncate(0)
dataFile.write(json.dumps(cards, sort_keys=True, indent=1, ensure_ascii=True))
dataFile.close()
