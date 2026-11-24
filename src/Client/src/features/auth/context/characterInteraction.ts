import { createContext, useContext } from 'react'

// حالة تفاعل بسيطة تربط تركيز حقل اسم الحساب بالشخصيتين في خلفية شاشة الدخول.
export interface CharacterInteractionValue {
  isInputFocused: boolean
  setInputFocused: (focused: boolean) => void
}

const noop = () => {}

export const CharacterInteractionContext = createContext<CharacterInteractionValue>({
  isInputFocused: false,
  setInputFocused: noop,
})

export function useCharacterInteraction(): CharacterInteractionValue {
  return useContext(CharacterInteractionContext)
}
