import { forwardRef, useEffect, useId, useRef, useState, type ComponentPropsWithoutRef } from 'react'
import { Check, ChevronDown } from 'lucide-react'
import { cn } from '@/lib/utils'

export interface SelectOption {
  value: string
  label: string
  disabled?: boolean
}

interface SelectProps extends Omit<ComponentPropsWithoutRef<'button'>, 'children' | 'onChange'> {
  options: SelectOption[]
  value: string
  onValueChange: (value: string) => void
}

function nextEnabledIndex(options: SelectOption[], start: number, direction: 1 | -1) {
  for (let offset = 0; offset < options.length; offset += 1) {
    const index = (start + direction * (offset + 1) + options.length) % options.length
    if (options[index] && !options[index].disabled) return index
  }
  return -1
}

export const Select = forwardRef<HTMLButtonElement, SelectProps>(
  function Select(
    {
      className,
      options,
      value,
      onValueChange,
      onBlur,
      onClick,
      onKeyDown,
      disabled,
      ...buttonProps
    },
    ref,
  ) {
    const listboxId = useId()
    const rootRef = useRef<HTMLSpanElement>(null)
    const optionRefs = useRef<Array<HTMLDivElement | null>>([])
    const [open, setOpen] = useState(false)
    const [activeIndex, setActiveIndex] = useState(-1)
    const selectedOption = options.find((option) => option.value === value)
    const initialActiveIndex = () => {
      const selectedIndex = options.findIndex((option) => option.value === value && !option.disabled)
      return selectedIndex >= 0 ? selectedIndex : options.findIndex((option) => !option.disabled)
    }

    useEffect(() => {
      if (!open) return

      const closeOnOutsideClick = (event: PointerEvent) => {
        if (event.target instanceof Node && !rootRef.current?.contains(event.target)) {
          setOpen(false)
        }
      }

      document.addEventListener('pointerdown', closeOnOutsideClick)
      return () => document.removeEventListener('pointerdown', closeOnOutsideClick)
    }, [open])

    useEffect(() => {
      const activeOption = optionRefs.current[activeIndex]
      if (open && activeOption && typeof activeOption.scrollIntoView === 'function') {
        activeOption.scrollIntoView({ block: 'nearest' })
      }
    }, [activeIndex, open])

    const close = () => setOpen(false)
    const choose = (option: SelectOption) => {
      if (option.disabled) return
      onValueChange(option.value)
      close()
    }

    return (
      <span ref={rootRef} className="relative block">
        <button
          {...buttonProps}
          ref={ref}
          type="button"
          role="combobox"
          aria-haspopup="listbox"
          aria-expanded={open}
          aria-controls={listboxId}
          aria-activedescendant={open && activeIndex >= 0 ? `${listboxId}-${activeIndex}` : undefined}
          disabled={disabled}
          className={cn(
            'flex h-10 w-full items-center justify-between gap-3 rounded-lg border border-slate-200 bg-white px-3 text-start text-sm text-slate-700 shadow-sm outline-none transition-colors hover:border-teal-300 focus-visible:border-teal-600 focus-visible:ring-4 focus-visible:ring-teal-600/10 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:text-slate-400',
            className,
          )}
          onBlur={(event) => {
            close()
            onBlur?.(event)
          }}
          onClick={(event) => {
            onClick?.(event)
            if (event.defaultPrevented) return
            if (open) {
              close()
              return
            }
            setActiveIndex(initialActiveIndex())
            setOpen(true)
          }}
          onKeyDown={(event) => {
            onKeyDown?.(event)
            if (event.defaultPrevented) return

            if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
              event.preventDefault()
              const direction = event.key === 'ArrowDown' ? 1 : -1
              if (!open) {
                const selectedIndex = options.findIndex(
                  (option) => option.value === value && !option.disabled,
                )
                setActiveIndex(
                  selectedIndex >= 0
                    ? nextEnabledIndex(options, selectedIndex, direction)
                    : initialActiveIndex(),
                )
                setOpen(true)
              } else {
                let start = activeIndex
                if (start < 0) start = direction === 1 ? options.length - 1 : 0
                setActiveIndex(nextEnabledIndex(options, start, direction))
              }
              return
            }

            if (event.key === 'Home' || event.key === 'End') {
              event.preventDefault()
              setOpen(true)
              const indices = options
                .map((option, index) => option.disabled ? -1 : index)
                .filter((index) => index >= 0)
              const lastIndex = indices[indices.length - 1] ?? -1
              setActiveIndex(event.key === 'Home' ? (indices[0] ?? -1) : lastIndex)
              return
            }

            if (event.key === 'Escape') {
              close()
              return
            }

            if (event.key === 'Enter' || event.key === ' ') {
              event.preventDefault()
              if (!open) {
                setActiveIndex(initialActiveIndex())
                setOpen(true)
              } else if (activeIndex >= 0) {
                const activeOption = options[activeIndex]
                if (activeOption) choose(activeOption)
              }
            }
          }}
        >
          <span className="truncate">{selectedOption?.label ?? ''}</span>
          <ChevronDown
            aria-hidden="true"
            className={cn('size-4 shrink-0 text-slate-400 transition-transform', open && 'rotate-180')}
          />
        </button>
        {open && (
          <div
            id={listboxId}
            role="listbox"
            className="absolute inset-x-0 top-[calc(100%+0.375rem)] z-50 max-h-64 overflow-y-auto rounded-xl border border-slate-200 bg-white p-1.5 shadow-lg shadow-slate-900/10"
          >
            {options.map((option, index) => {
              const selected = option.value === value
              const active = index === activeIndex
              return (
                <div
                  id={`${listboxId}-${index}`}
                  key={option.value}
                  ref={(element) => {
                    optionRefs.current[index] = element
                  }}
                  role="option"
                  aria-selected={selected}
                  aria-disabled={option.disabled || undefined}
                  className={cn(
                    'flex min-h-9 cursor-pointer items-center justify-between gap-3 rounded-lg px-3 py-2 text-sm transition-colors',
                    selected ? 'bg-teal-50 font-medium text-teal-800' : 'text-slate-700',
                    active && !selected && 'bg-slate-100',
                    option.disabled && 'cursor-not-allowed opacity-45',
                  )}
                  onMouseDown={(event) => event.preventDefault()}
                  onMouseMove={() => !option.disabled && setActiveIndex(index)}
                  onClick={() => choose(option)}
                >
                  <span>{option.label}</span>
                  {selected && <Check aria-hidden="true" className="size-4 text-teal-700" />}
                </div>
              )
            })}
          </div>
        )}
      </span>
    )
  },
)
