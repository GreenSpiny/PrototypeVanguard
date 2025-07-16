const output = {set: '', cards: []};

const headArea = document.getElementsByClassName("cardlist_head")[0].firstElementChild;
const headText = headArea.innerHTML;
const setName = headText.substring(headText.indexOf('[') + 1, headText.indexOf(']'));
output.set = setName;

const cardArea = document.getElementById("cardlist-container").firstElementChild;
for (const child of cardArea.children) {
    let a = child.getElementsByTagName("a")[0];
    let url = 'https://en.cf-vanguard.com' + a.getAttribute("href");
    url = url.substring(0, url.indexOf('&expansion='));
    output.cards.push(url);
}

const json = JSON.stringify(output, null, 1);
console.log(json);