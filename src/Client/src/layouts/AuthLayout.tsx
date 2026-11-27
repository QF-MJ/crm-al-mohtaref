import { useState } from 'react'
import { Navigate, Outlet } from 'react-router-dom'
import { Languages, ShieldCheck } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import logo from '@/assets/brand/logo.png'
import { ThemeToggle } from '@/components/common/ThemeToggle'
import { CharacterInteractionContext } from '@/features/auth/context/characterInteraction'
import { useAuthStore } from '@/stores/auth.store'
import '@/styles/login.css'

// شاشة دخول Clean Minimalist: بطاقة واحدة هادئة على خلفية ناعمة،
// مع ترويسة بسيطة (شعار + تبديل اللغة/الثيم) وتذييل صغير.

// الشخصيتان تخرجان من خلف الزاويتين العلويتين للكرت: الفتاة يميناً والشاب يساراً.
// التلاشي في الأسفل عبر mask-image بدل أي قصّ حاد، والحركة نابضة وسلسة.
const PEEK_WRAPPER =
  'pointer-events-none absolute top-4 z-0 h-0 w-[42%] transition-all duration-500 ease-[cubic-bezier(0.34,1.56,0.64,1)] [will-change:translate,rotate] motion-reduce:transition-none sm:top-6'

const PEEK_IMAGE =
  'absolute inset-0 h-full w-full select-none object-contain drop-shadow-[0_16px_24px_rgba(15,23,42,0.18)] [mask-image:linear-gradient(to_bottom,black_70%,transparent_100%)] [-webkit-mask-image:linear-gradient(to_bottom,black_70%,transparent_100%)]'

// التفعيل بالماوس، وعند اللمس/التركيز داخل النموذج على الموبايل
const PEEK_SHOW_RIGHT =
  'group-hover:translate-x-28 group-focus-within:translate-x-28 group-active:translate-x-28 group-hover:-translate-y-4 group-focus-within:-translate-y-4 group-active:-translate-y-4 group-hover:rotate-12 group-focus-within:rotate-12 group-active:rotate-12 max-sm:group-hover:translate-x-16 max-sm:group-focus-within:translate-x-16 max-sm:group-active:translate-x-16'

const PEEK_SHOW_LEFT =
  'group-hover:-translate-x-28 group-focus-within:-translate-x-28 group-active:-translate-x-28 group-hover:-translate-y-4 group-focus-within:-translate-y-4 group-active:-translate-y-4 group-hover:-rotate-12 group-focus-within:-rotate-12 group-active:-rotate-12 max-sm:group-hover:-translate-x-16 max-sm:group-focus-within:-translate-x-16 max-sm:group-active:-translate-x-16'

// عند التركيز على حقل اسم الحساب: بروز إضافي 25px ونزول نحو مستوى الحقل
const PEEK_FOCUS_RIGHT =
  'translate-x-[157px] translate-y-8 rotate-12 max-sm:translate-x-[109px] max-sm:translate-y-6'
const PEEK_FOCUS_LEFT =
  '-translate-x-[157px] translate-y-8 -rotate-12 max-sm:-translate-x-[109px] max-sm:translate-y-6'

// فقاعة كلام تظهر فوق رأس الشخصية: تبقى فوق حافة الكرت تماماً حتى لا يحجبها الكرت،
// وتُحصَر داخل عرض الشاشة على الموبايل بسقف max-w.
const PEEK_BUBBLE =
  'pointer-events-none absolute bottom-full z-[2] w-max max-w-[200px] -translate-x-1/2 rounded-lg bg-white px-3 py-2 text-center text-xs font-medium leading-5 text-slate-700 shadow-md ring-1 ring-slate-900/5 transition-all duration-500 ease-[cubic-bezier(0.34,1.56,0.64,1)] motion-reduce:transition-none dark:bg-slate-800 dark:text-slate-100 dark:ring-white/10 max-sm:max-w-[120px] max-sm:px-2.5 max-sm:text-[11px]'

const PEEK_BUBBLE_ARROW =
  'absolute -bottom-1 left-1/2 size-2.5 -translate-x-1/2 rotate-45 rounded-[2px] bg-white shadow-[1px_1px_2px_rgba(15,23,42,0.06)] dark:bg-slate-800'

// الحالة الافتراضية مخفية ومصغّرة، ثم تنبثق بعد خروج الشخصية بقليل
const PEEK_BUBBLE_PEEK =
  'mb-8 scale-75 opacity-0 group-hover:scale-100 group-hover:opacity-100 group-hover:delay-150 group-focus-within:scale-100 group-focus-within:opacity-100 group-focus-within:delay-150 group-active:scale-100 group-active:opacity-100 group-active:delay-150'

// عند التركيز تنزل الشخصية 32px، لذلك نرفع الفقاعة أكثر لتظل فوق الرأس
const PEEK_BUBBLE_FOCUS = 'mb-16 scale-100 opacity-100 delay-150'

interface SpeechBubbleProps {
  text: string
  side: 'right' | 'left'
  visible: boolean
}

function SpeechBubble({ text, side, visible }: SpeechBubbleProps) {
  const isRight = side === 'right'
  // مواضع مختلفة قليلاً عند التركيز لأن الشخصية تخرج أبعد، لضمان بقاء الفقاعة داخل الشاشة
  const position = visible
    ? isRight
      ? 'left-[15%] max-sm:-left-2'
      : 'left-[85%] max-sm:left-[108%]'
    : isRight
      ? 'left-[35%] max-sm:left-[10%]'
      : 'left-[65%] max-sm:left-[90%]'
  // عكس ميلان الحاوية حتى تبقى الفقاعة مستقيمة وسهلة القراءة
  const counterRotate = visible
    ? isRight
      ? '-rotate-12'
      : 'rotate-12'
    : isRight
      ? 'group-hover:-rotate-12 group-focus-within:-rotate-12 group-active:-rotate-12'
      : 'group-hover:rotate-12 group-focus-within:rotate-12 group-active:rotate-12'

  return (
    <div
      aria-hidden="true"
      className={`${PEEK_BUBBLE} ${position} ${counterRotate} ${visible ? PEEK_BUBBLE_FOCUS : PEEK_BUBBLE_PEEK}`}
    >
      {text}
      <span aria-hidden="true" className={PEEK_BUBBLE_ARROW} />
    </div>
  )
}

export function AuthLayout() {
  const { t, i18n } = useTranslation()
  const { t: ta } = useTranslation('auth')
  const user = useAuthStore((state) => state.user)
  const [isInputFocused, setIsInputFocused] = useState(false)
  if (user) return <Navigate to="/" replace />

  const nextLanguage = i18n.language.startsWith('ar') ? 'en' : 'ar'

  return (
    <main className="relative flex min-h-[100svh] flex-col overflow-x-clip bg-[#f5f7f8]">
      <div aria-hidden="true" className="login-canvas pointer-events-none absolute inset-0" />

      <header className="relative z-10 flex items-center justify-between gap-3 px-5 py-4 sm:px-8 sm:py-5">
        <div className="flex min-w-0 items-center gap-3">
          <img src={logo} alt={ta('logoAlt')} className="app-logo h-8 w-auto" />
          <span aria-hidden="true" className="hidden h-6 w-px bg-border sm:block" />
          <span className="hidden truncate text-xs font-medium text-muted-foreground sm:block">
            {ta('employeePortal')}
          </span>
        </div>

        <div className="flex items-center gap-1">
          <button
            type="button"
            onClick={() => void i18n.changeLanguage(nextLanguage)}
            className="inline-flex h-9 items-center gap-1.5 rounded-md px-2.5 text-xs font-medium text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/40"
            aria-label={t('actions.switchLanguage')}
          >
            <Languages aria-hidden="true" className="size-4" />
            <span>{t(nextLanguage === 'ar' ? 'actions.arabic' : 'actions.english')}</span>
          </button>
          <ThemeToggle />
        </div>
      </header>

      <div className="relative z-10 flex flex-1 items-center justify-center px-4 pb-6 pt-2 sm:pb-10">
        <div className="w-full max-w-[420px]">
          <div className="group relative isolate">
            {/* الفتاة: تطلّ من الجانب الأيمن */}
            <div
              aria-hidden="true"
              className={`${PEEK_WRAPPER} right-0 ${isInputFocused ? PEEK_FOCUS_RIGHT : PEEK_SHOW_RIGHT}`}
            >
              <div className="relative ml-auto aspect-square h-[200px] max-sm:h-[150px]">
                <img
                  src="/login/character-right.png"
                  alt=""
                  draggable={false}
                  className={PEEK_IMAGE}
                />
                <SpeechBubble text={ta('peekGirl')} side="right" visible={isInputFocused} />
              </div>
            </div>

            {/* الشاب: يطلّ من الجانب الأيسر */}
            <div
              aria-hidden="true"
              className={`${PEEK_WRAPPER} left-0 ${isInputFocused ? PEEK_FOCUS_LEFT : PEEK_SHOW_LEFT}`}
            >
              <div className="relative aspect-[344/378] h-[200px] max-sm:h-[150px]">
                <img
                  src="/login/character-left.png"
                  alt=""
                  draggable={false}
                  className={PEEK_IMAGE}
                />
                <SpeechBubble text={ta('peekBoy')} side="left" visible={isInputFocused} />
              </div>
            </div>

            <div className="relative z-10 overflow-hidden rounded-2xl border border-border bg-card p-6 shadow-[0_20px_55px_-38px_rgba(11,60,55,0.45)] sm:p-8">
              <span
                aria-hidden="true"
                className="pointer-events-none absolute inset-x-12 top-0 h-px bg-gradient-to-r from-transparent via-primary/45 to-transparent"
              />
              <CharacterInteractionContext.Provider
                value={{
                  isInputFocused,
                  setInputFocused: setIsInputFocused,
                }}
              >
                <Outlet />
              </CharacterInteractionContext.Provider>
            </div>
          </div>
        </div>
      </div>

      <footer className="relative z-10 flex flex-wrap items-center justify-center gap-x-2 gap-y-1 px-6 pb-5 text-[11px] text-muted-foreground sm:pb-6">
        <span>{ta('copyright')}</span>
        <span aria-hidden="true" className="hidden h-3 w-px bg-border sm:block" />
        <span className="inline-flex items-center gap-1.5">
          <ShieldCheck aria-hidden="true" className="size-3.5" />
          {ta('secureLabel')}
        </span>
      </footer>
    </main>
  )
}
