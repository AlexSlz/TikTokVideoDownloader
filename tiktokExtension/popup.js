setInterval(loadData, 300)
function loadData() {
  chrome.storage.local.get('videoStorage', function (result) {
    if (result.videoStorage != undefined) {
      removeOptions()
      result.videoStorage.forEach((item) => {
        if (item !== null) addLinkTolist(item.title, item.link)
      })
      document.getElementById('videoCount').innerHTML = document.getElementById('listUrlVideo').length
    }
  })
  chrome.storage.local.get('checkboxStorage', function (result) {
    if (result.checkboxStorage != undefined) {
      document.getElementById('myCheckBox').checked = result.checkboxStorage
    }
  })

  var m = document.getElementById('m')

  m.oninput = function () {
    chrome.storage.local.set({ mStorage: m.value })
  }
  chrome.storage.local.get('mStorage', function (result) {
    if (result.mStorage != undefined) {
      m.value = result.mStorage
    }
  })
  var k = document.getElementById('k')

  k.oninput = function () {
    chrome.storage.local.set({ kStorage: k.value })
  }
  chrome.storage.local.get('kStorage', function (result) {
    if (result.kStorage != undefined) {
      k.value = result.kStorage
    }
  })

  var time = document.getElementById('time')

  time.oninput = function () {
    chrome.storage.local.set({ timeStorage: time.value })
  }
  chrome.storage.local.get('timeStorage', function (result) {
    if (result.timeStorage != undefined) {
      time.value = result.timeStorage
    }
  })
}

function removeOptions() {
  var i,
    L = document.getElementById('listUrlVideo').options.length - 1
  for (i = L; i >= 0; i--) {
    document.getElementById('listUrlVideo').remove(i)
  }
}

function addLinkTolist(name, link) {
  let opt = document.createElement('option')
  document.getElementById('listUrlVideo').options.add(opt)
  opt.text = name
  opt.value = link
}
document.getElementById('remove').addEventListener('click', () => {
  chrome.storage.local.clear(() => {
    console.log('Everything was removed')
  })
})

document.getElementById('Download').addEventListener('click', () => {
  download('video.txt')
})
function download(filename) {
  var element = document.createElement('a')

  let text = ''
  for (var item of document.getElementById('listUrlVideo').options) {
    text += item.value + '\n'
  }

  element.setAttribute('href', 'data:text/plain;charset=utf-8,' + text)
  element.setAttribute('download', filename)
  element.style.display = 'none'
  document.body.appendChild(element)
  element.click()
  document.body.removeChild(element)
}

document.getElementById('myCheckBox').addEventListener('change', () => {
  chrome.storage.local.set({ checkboxStorage: document.getElementById('myCheckBox').checked })
  sendMessage('run')
})

function sendMessage(message) {
  chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
    chrome.tabs.sendMessage(tabs[0].id, message)
  })
}
