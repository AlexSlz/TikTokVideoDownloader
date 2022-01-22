let errorLink =
  'https://v16m-default.akamaized.net/6bc248ce934e69f2872963afc63ce013/61e7795d/video/tos/alisg/tos-alisg-pv-0037c001/74a970e3eab6427292567a99558d77d6/?a=0&br=4100&bt=2050&cd=0|0|0&ch=0&cr=0&cs=&dr=0&ds=3&er=&ft=.cwOVIIL7O2WH6BhlbmiLo&l=2022011820370201024404224324775E34&lr=all&mime_type=video_mp4&net=0&pl=0&qs=13&rc=amg0OGc6ZjpuNzMzODczNEApc3hrOng6bTQ4ZjMzajczeWdtcmU0cjRnLjJgLS1kMS1zc2RiLjJhNGA0aDEtLTIxLS46Yw==&vl=&vr='

let contextMenuItem = [
  {
    'id': 'video',
    'title': 'AddVideo',
    'contexts': ['video'],
  },
]

loadData()
function loadData() {
  chrome.contextMenus.removeAll()
  contextMenuItem.forEach((item) => {
    chrome.contextMenus.create(item)
  })
}

chrome.contextMenus.onClicked.addListener((data) => {
  if (data.menuItemId.includes('video')) {
    console.log(data)
    updateStorage(data)
  }
})

let localVideoStorage = []
// Обновить данные в локальном хранилище
async function updateStorage(link) {
  localVideoStorage = (await getDataFromStorage('videoStorage')) || []
  let json = await response(link.pageUrl, link.srcUrl)
  let likeCount = 0
  for (let item of localVideoStorage) {
    if (item.title === json.title || json.link.includes(errorLink)) {
      likeCount++
      return 'The video is already in the list.....'
    }
  }
  if (likeCount <= 0) {
    localVideoStorage.push(json)
    chrome.storage.local.set({ videoStorage: localVideoStorage })
  }
  return 'GooD ViDeO :)'
}

function getDataFromStorage(key) {
  return new Promise((resolve, reject) => {
    try {
      chrome.storage.local.get(key, function (value) {
        resolve(value[key])
      })
    } catch (ex) {
      reject(ex)
    }
  })
}
// Запрос видео и названия с сайта
async function response(res, q) {
  let tryRun = 1
  let a
  if (res.includes('@')) {
    res = 'www.tiktok.com/@' + res.split('@')[1]
  }
  a = await fetch(`https://www.tikwm.com/api/?url=${res}`)
    .then((result) => result.json())
    .then((obj) => {
      console.log(res)
      if (obj.msg.includes('1 request/second.') && tryRun < 15) {
        return retryReq(res)
      } else return { title: res.split('@')[1] || Math.random(), link: obj.data?.play || q }
    })
  return a
}
async function retryReq(res) {
  await delay(1000)
  return response(res)
}
