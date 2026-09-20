import { NicknameSettings } from '../../features/roster/components/NicknameSettings'
import { PinColorSettings } from '../../features/roster/components/PinColorSettings'
import { PinMarkSettings } from '../../features/roster/components/PinMarkSettings'
import { SteamIdSettings } from '../../features/roster/components/SteamIdSettings'

export function SettingsPage() {
  return (
    <>
      <h1 className="text-2xl font-semibold">Ustawienia</h1>
      <NicknameSettings />
      <PinColorSettings />
      <PinMarkSettings />
      <SteamIdSettings />
    </>
  )
}
