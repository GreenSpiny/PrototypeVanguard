import json
import os

# Function 01: Turns the scraped data of the RAW URLS folder into a unified JSON file.
# This ensures it is sorted by the order of the folders and files supplied, with Structure Decks first, Normal Packs second, and Special Packs third.
def function01():
	allCards = []
	urlsFolder = os.path.join(os.getcwd(), 'Raw Urls')
	for subFolder in os.listdir(urlsFolder):
		subFolderPath = os.path.join(urlsFolder, subFolder)
		if os.path.isdir(subFolderPath):
			for file in os.listdir(subFolderPath):
				filePath = os.path.join(subFolderPath, file)
				if os.path.isfile(filePath):
					f = open(filePath, 'r')
					contents = json.loads(f.read())
					cardsArray = contents['cards']
					for card in cardsArray:
						allCards.append(card)
					f.close()
	outputpath = os.path.join(os.getcwd(),'allCards.json')
	outputJSON = json.dumps(allCards, indent=1)
	outputfile = open(outputpath, 'w')
	outputfile.write(outputJSON);
	outputfile.close()

function01();