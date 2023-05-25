const socket = new WebSocket("YOUR SOCKET"); 
const radioButtons = document.querySelectorAll("input[type=radio]");                                
const betButton = document.getElementById("betButton");

let userId;
let jwtToken;
let clientId;
let viewerName;


// betButton is disabled by default, start the round with the msg "startround"
betButton.disabled = "disabled";

// Check if the viewer is logged in or logged out
window.Twitch.ext.onAuthorized((auth) => 
{
  if (window.Twitch.ext.viewer.isLinked) 
  {
    // user is logged in
    document.body.classList.add('logged_in');
    document.body.classList.remove('logged_out');

    loadProfile();
  } 
  else 
  {
    document.body.classList.remove('logged_in');
    document.body.classList.add('logged_out');
  }
});

// loginButton
document.getElementById('loginButton').addEventListener('click', (e) => {
  e.preventDefault();

  window.Twitch.ext.actions.requestIdShare();
});

// Bet
radioButtons.forEach((radioButton) => {
  betButton.addEventListener("click", () => {
    if (radioButton.checked) 
    {
      const socket = new WebSocket("YOUR SOCKET");

      socket.onopen = () => {
        socket.send(JSON.stringify({
          action: "sendmessage",
          message: viewerName + ' ' + radioButton.value
        }));
      };
      betButton.disabled = true;
    }
  });
});

// startround msg -> bet enabled
socket.onmessage = (event) => {
  if (event.data == "startround") 
  {
    betButton.disabled = false;
  } 
  console.log("Message received: " + event.data);
};

// After login get the viewer userId, jwtToken, clientId and name 
function loadProfile() {
  // USER ID
  userId = window.Twitch.ext.viewer.id;
  // JWT TOKEN & CLIENT ID
  window.Twitch.ext.onAuthorized(function(auth) {
    jwtToken = auth.helixToken;
    clientId = auth.clientId;
  });
  // Name
  getViewerName();

  /*  
  console.log('User ID: ' + userId + '\n');
  console.log('JWT Token: ' + jwtToken + '\n');
  console.log('Client ID: ' + clientId + '\n');
  console.log('Name: ' + viewerName + '\n');
  */  
}

// Function to get the viewer login name with userId, jwtToken and clientId
function getViewerName() {
  const url = `https://api.twitch.tv/helix/users?id=${userId}`;

  const headers = {
    "Authorization": 'Extension ' + jwtToken,
    "Client-ID": clientId
  };

  fetch(
    url, 
    {
      headers: headers,
  })
  .then((response) => response.json())
  .then((data) => {
    viewerName = data.data[0].login;
  });
}

// [DEV] - Check if API is connected 
/*
socket.onopen = () => {
  console.log("Connection opened");
};
*/
// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
var doaImg = document.getElementById("doaImg");
var betForm = document.getElementById("bet");

betForm.addEventListener("change", function() {
  var selectedValue = this.querySelector('input[type="radio"]:checked').value;

  switch (selectedValue) {
    // ROWS
    case "row1":
      doaImg.src = "images/row1.png";
      break;
    case "row2":
      doaImg.src = "images/row2.png";
      break;
    case "row3":
      doaImg.src = "images/row3.png";
      break;
    case "row4":
      doaImg.src = "images/row4.png";
      break;
    // COLUMNS
    case "column2":
      doaImg.src = "images/column2.png";
      break;
    case "column3":
      doaImg.src = "images/column3.png";
      break;
    case "column4":
      doaImg.src = "images/column4.png";
      break;
    case "column5":
      doaImg.src = "images/column5.png";
      break;
    case "column6":
      doaImg.src = "images/column6.png";
      break;
    case "column7":
      doaImg.src = "images/column7.png";
      break;
    case "column8":
      doaImg.src = "images/column8.png";
      break;
    case "column9":
      doaImg.src = "images/column9.png";
      break;
    case "column10":
      doaImg.src = "images/column10.png";
      break;
    case "columnj":
      doaImg.src = "images/columnj.png";
      break;
    case "columnq":
      doaImg.src = "images/columnq.png";
      break;
    case "columnk":
      doaImg.src = "images/columnk.png";
      break;
    case "columna":
      doaImg.src = "images/columna.png";
      break;
    // COMBINATIONS
    case "comb12":
      doaImg.src = "images/comb12.png";
      break;
    case "comb13":
      doaImg.src = "images/comb13.png";
      break;
    case "comb14":
      doaImg.src = "images/comb14.png";
      break;
    case "comb23":
      doaImg.src = "images/comb23.png";
      break;
    case "comb24":
      doaImg.src = "images/comb24.png";
      break;
    case "comb34":
      doaImg.src = "images/comb34.png";
      break;
    case "even":
      doaImg.src = "images/even.png";
      break;
    case "odd":
      doaImg.src = "images/odd.png";
      break;
    case "num":
      doaImg.src = "images/num.png";
      break;
    case "face":
      doaImg.src = "images/face.png";
      break;
    case "all":
      doaImg.src = "images/all.png";
      break;
  }
});