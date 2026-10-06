import { memo, useId } from 'react'
import { useOpponents } from '../hooks/useOpponents'

interface OpponentNameInputProps {
  value: string
  onChange: (value: string) => void
  required?: boolean
  placeholder?: string
  className?: string
}

/** Free-text opponent name with suggestions of already-known teams, so "Team X" isn't typed three different ways across
 * notes, results and events (the profile merges spellings case-insensitively, but not typos). */
export const OpponentNameInput = memo(function OpponentNameInput({
  value,
  onChange,
  required,
  placeholder = 'Nazwa przeciwnika',
  className,
}: OpponentNameInputProps) {
  const listId = useId()
  const { data: opponents } = useOpponents()

  return (
    <>
      <input
        required={required}
        maxLength={100}
        list={listId}
        autoComplete="off"
        placeholder={placeholder}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className={className}
      />
      <datalist id={listId}>
        {opponents?.map((opponent) => (
          <option key={opponent.name} value={opponent.name} />
        ))}
      </datalist>
    </>
  )
})
