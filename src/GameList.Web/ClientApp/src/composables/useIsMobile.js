import { ref, onMounted, onBeforeUnmount } from 'vue'

const MOBILE_BREAKPOINT = 640
const subscribers = new Set()
const isMobileGlobal = ref(typeof window !== 'undefined' ? window.innerWidth < MOBILE_BREAKPOINT : false)

let listening = false
let debounceTimer = null

function handleResize() {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    isMobileGlobal.value = window.innerWidth < MOBILE_BREAKPOINT
  }, 100)
}

function subscribe(id) {
  subscribers.add(id)
  if (!listening) {
    window.addEventListener('resize', handleResize)
    listening = true
  }
}

function unsubscribe(id) {
  subscribers.delete(id)
  if (subscribers.size === 0 && listening) {
    window.removeEventListener('resize', handleResize)
    clearTimeout(debounceTimer)
    listening = false
  }
}

let nextId = 0

export function useIsMobile() {
  const id = nextId++

  onMounted(() => subscribe(id))
  onBeforeUnmount(() => unsubscribe(id))

  return isMobileGlobal
}
