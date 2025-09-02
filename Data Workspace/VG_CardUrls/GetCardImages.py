import sys
import os
import urllib.request
import json
import math

if __name__ == "__main__":

	dataFile = open('cardsData.json', 'r')
	data = json.loads(dataFile.read())
	cards = data["cards"]
	dataFile.close()

	startIndex = 0;
	endIndex = len(cards);
	if len(sys.argv) >= 2:
		startIndex = int(sys.argv[1])
	if len(sys.argv) >= 3:
		endIndex = int(sys.argv[2])

	for card in cards.values():
		index = card['index']
		folder = str(math.floor(index / 100.0))


		if index >= startIndex and index < endIndex:
			url = card['image']
			path = os.path.join(os.getcwd(), "images")
			path = os.path.join(path, folder)
			if not os.path.exists(path):
				os.mkdir(path)
			path = os.path.join(path, str(index) + ".png")
			urllib.request.urlretrieve(url, path)