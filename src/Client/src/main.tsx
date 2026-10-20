import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import './styles/brand.css'
import App from '@/app/App'
import { setupInterceptors } from '@/api/interceptors'
import { env } from '@/lib/env'

// MSW يشتغل فقط عند VITE_USE_MOCKS=true وبوضع التطوير
async function enableMocking() {
  if (!import.meta.env.DEV || !env.useMocks) return
  const { worker } = await import('@/mocks/browser')
  await worker.start({ onUnhandledFrame: 'bypass' })
}

setupInterceptors()

void enableMocking().then(() => {
  createRoot(document.getElementById('root')!).render(
    <StrictMode>
      <App />
    </StrictMode>,
  )
})
