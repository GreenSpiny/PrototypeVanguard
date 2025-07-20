import sys
import json
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.options import Options

def GetCardInformation(driver, url):

	# get card information
	driver.get(url)
	content = driver.find_element(By.CLASS_NAME, 'cardlist_detail')

	v_name = content.find_element(By.CLASS_NAME, 'face').get_attribute('innerHTML').strip()
	v_image = content.find_element(By.TAG_NAME,'img').get_attribute('src').strip()
	v_type = content.find_element(By.CLASS_NAME, 'type').get_attribute('innerHTML').strip()
	v_nation = content.find_element(By.CLASS_NAME, 'nation').get_attribute('innerHTML').strip()
	v_race = content.find_element(By.CLASS_NAME, 'race').get_attribute('innerHTML').strip()
	v_grade = content.find_element(By.CLASS_NAME, 'grade').get_attribute('innerHTML').strip()
	v_power = content.find_element(By.CLASS_NAME, 'power').get_attribute('innerHTML').strip()
	v_critical = content.find_element(By.CLASS_NAME, 'critical').get_attribute('innerHTML').strip()
	v_shield = content.find_element(By.CLASS_NAME, 'shield').get_attribute('innerHTML').strip()
	v_skill = content.find_element(By.CLASS_NAME, 'skill').get_attribute('innerHTML').strip()
	v_effect = content.find_element(By.CLASS_NAME, 'effect').get_attribute('innerHTML').strip()
	v_regulation = content.find_element(By.CLASS_NAME, 'regulation').get_attribute('innerHTML').strip()

	# parse additional information
	v_id = "".join(x for x in v_name if x.isalnum())
	v_id = v_id.lower()

	if ' ' in v_grade:
		v_grade = v_grade.split(' ')[1]
	if not v_grade.isnumeric():
		v_grade = '0'

	if ' ' in v_power:
		v_power = v_power.split(' ')[1]
	if not v_power.isnumeric():
		v_power = '0'

	if ' ' in v_critical:
		v_critical = v_critical.split(' ')[1]
	if not v_critical.isnumeric():
		v_critical = '0'

	if ' ' in v_shield:
		v_shield = v_shield.split(' ')[1]
	if not v_shield.isnumeric():
		v_shield = '0'

	cardData = {
		"id": v_id,
		"name": v_name,
		"image": v_image,
		"type": v_type,
		"nation": v_nation,
		"race": v_race,
		"grade": int(v_grade),
		"power": int(v_power),
		"critical": int(v_critical),
		"shield": int(v_shield),
		"skill": v_skill,
		"effect": v_effect,
		"regulation": v_regulation
	}

	return cardData;

if __name__ == "__main__":

	# parse the results file
	resultsFile = open('allCardsSingleton.json', 'r+')
	resultsData = json.loads(resultsFile.read())
	resultsDataCards = resultsData['cards']

	# parse the card URLs file
	# accepts start and / or end indicies 
	urlsFile = open('allCards.json', 'r')
	urlsData = json.loads(urlsFile.read())
	urlsFile.close()
	startIndex = 0;
	endIndex = len(urlsData);
	if len(sys.argv) >= 2:
		startIndex = int(sys.argv[1])
	if len(sys.argv) >= 3:
		endIndex = int(sys.argv[2])

	# open the driver
	options = Options()
	options.add_argument("--headless=new")
	driver = webdriver.Chrome(options=options)

	# get card information
	for i in range (startIndex, endIndex):
		cardData = GetCardInformation(driver, urlsData[i])
		print(cardData['id'])
		if cardData['id'] not in resultsDataCards:
			resultsDataCards[cardData['id']] = cardData

	# close the driver
	driver.close()

	# write the results to disk
	resultsJSON = json.dumps(resultsData, sort_keys=True, indent=1)
	resultsFile.seek(0)
	resultsFile.truncate(0)
	resultsFile.write(resultsJSON);
	resultsFile.close()