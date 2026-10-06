import { PageHeader } from '../../components/ui/PageHeader'
import { NicknameSettings } from '../../features/roster/components/NicknameSettings'
import { PinColorSettings } from '../../features/roster/components/PinColorSettings'
import { PinMarkSettings } from '../../features/roster/components/PinMarkSettings'
import { SteamIdSettings } from '../../features/roster/components/SteamIdSettings'

export function SettingsPage() {
  return (
    <>
      <PageHeader title="Ustawienia" />
      <div className="grid items-start gap-6 lg:grid-cols-2">
        <NicknameSettings />
        <PinColorSettings />
        <PinMarkSettings />
        <SteamIdSettings />
      </div>
    </>
  )
}
