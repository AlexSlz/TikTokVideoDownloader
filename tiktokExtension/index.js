let errorLink =
  'https://v16m-default.akamaized.net/6bc248ce934e69f2872963afc63ce013/61e7795d/video/tos/alisg/tos-alisg-pv-0037c001/74a970e3eab6427292567a99558d77d6/?a=0&br=4100&bt=2050&cd=0|0|0&ch=0&cr=0&cs=&dr=0&ds=3&er=&ft=.cwOVIIL7O2WH6BhlbmiLo&l=2022011820370201024404224324775E34&lr=all&mime_type=video_mp4&net=0&pl=0&qs=13&rc=amg0OGc6ZjpuNzMzODczNEApc3hrOng6bTQ4ZjMzajczeWdtcmU0cjRnLjJgLS1kMS1zc2RiLjJhNGA0aDEtLTIxLS46Yw==&vl=&vr='
// Получение данных из popup.js
chrome.runtime.onMessage.addListener((obj) => {
  if (obj === 'run') {
    startWathVideo()
  }
})

async function startWathVideo(count = 1000, wait = 1000, minM = 1, minK = 10) {
  let obj = await CheckData()
  console.log(obj)
  while (obj.run) {
    obj = await CheckData()
    try {
      let like
      try {
        like = document.querySelector('[data-e2e="browse-like-count"]').innerHTML
      } catch {
        like = document.querySelector('[data-e2e="like-count"]').innerHTML
      }
      let time
      document.querySelectorAll('div').forEach((item) => {
        if (item.className.includes('DivSeekBarTimeContainer')) time = item.innerHTML.split('/')[1].split(':')
      })
      if (
        ((parseFloat(like) > obj.k && like.includes('K')) || (parseFloat(like) > obj.m && like.includes('M'))) &&
        parseInt(time[0] + time[1]) <= obj.time
      ) {
        var a = await updateStorage(window.location.href)
        console.log(a)
      }
    } catch (e) {}
    // Остановка если нет видео
    if (nextVideo()) {
      chrome.storage.local.set({ checkboxStorage: false })
      break
    }
    await delay(wait)
  }
}

async function CheckData() {
  let tempRun = await getDataFromStorage('checkboxStorage')
  let tempM = await getDataFromStorage('mStorage')
  let tempK = await getDataFromStorage('kStorage')
  let tempTime = await getDataFromStorage('timeStorage')
  return { run: tempRun, m: tempM, k: tempK, time: tempTime }
}

async function getDataFromStorage(key) {
  return new Promise((resolve, reject) => {
    try {
      chrome.storage.local.get(key, function (value) {
        resolve(value[key])
      })
    } catch (ex) {
      console.error(ex)
      reject([])
    }
  })
}

// Перейти к следующему видео
function nextVideo() {
  let next = document.querySelector('[data-e2e="arrow-right"]')
  if (next !== null) {
    if (!next.hasAttribute('disabled')) {
      next.click()
    } else {
      alert('Where button')
      return true
    }
  } else {
    return true
  }
}

const delay = async (ms) => new Promise((resolve) => setTimeout(resolve, ms))

let localVideoStorage = []
// Обновить данные в локальном хранилище
async function updateStorage(link) {
  localVideoStorage = (await getDataFromStorage('videoStorage')) || []
  let json = await response(link)
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

// Запрос видео и названия с сайта
async function response(res) {
  let tryRun = 1
  let a
  if (res.includes('@')) {
    res = 'www.tiktok.com/@' + res.split('@')[1]
  }
  a = await fetch(`https://www.tikwm.com/api/?url=${res}`)
    .then((result) => result.json())
    .then((obj) => {
      if (obj.msg.includes('1 request/second.') && tryRun < 15) {
        return retryReq(res)
      } else return { title: res.split('@')[1], link: obj.data?.play || document.querySelector('video').src }
    })
  return a
}
async function retryReq(res) {
  await delay(1000)
  return response(res)
}
