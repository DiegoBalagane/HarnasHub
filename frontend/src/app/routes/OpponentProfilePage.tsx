import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { useIsCoachOrManager } from '../../features/auth/hooks/useIsCoachOrManager'
import {
  MapRecords,
  MatchHistory,
  UpcomingGames,
} from '../../features/opponents/components/OpponentMatchSections'
import { OpponentActionsMenu } from '../../features/opponents/components/OpponentActionsMenu'
import { OpponentNotesSection } from '../../features/opponents/components/OpponentNotesSection'
import { RecordBadge } from '../../features/opponents/components/RecordBadge'
import { useOpponentProfile } from '../../features/opponents/hooks/useOpponents'
import { opponentProfilePath, opponentReportPath } from '../../features/opponents/paths'
import { VetoSuggestionPanel } from '../../features/veto/components/VetoSuggestionPanel'

/** One opponent's full picture: record, per-map results, upcoming games, match history and scouting notes. */
export function OpponentProfilePage() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const name = searchParams.get('name') ?? ''
  const canManage = useIsCoachOrManager()
  const { data: profile, isLoading, isError } = useOpponentProfile(name)

  return (
    <div className="flex w-full flex-col gap-6">
      <Link to="/opponents" className="self-start text-sm text-neutral-400 hover:text-white">
        ← Wszyscy przeciwnicy
      </Link>

      {name.trim() === '' && <p className="text-neutral-400">Nie wybrano przeciwnika.</p>}
      {isLoading && <p className="text-neutral-400">Ładowanie profilu…</p>}
      {isError && <p className="text-danger-400">Nie udało się pobrać profilu przeciwnika.</p>}

      {profile && (
        <>
          <header className="flex flex-wrap items-baseline justify-between gap-2">
            <h1 className="text-2xl font-semibold">
              {profile.name}
              {profile.isHidden && <span className="ml-2 text-sm font-normal text-neutral-500">ukryty</span>}
            </h1>
            <RecordBadge wins={profile.wins} losses={profile.losses} draws={profile.draws} />
            <Link to={opponentReportPath(profile.name)} className="text-sm text-primary-300 hover:underline">
              Raport FACEIT: oni vs my →
            </Link>
            {canManage && (
              <OpponentActionsMenu
                name={profile.name}
                isHidden={profile.isHidden}
                onRenamed={(newName) => navigate(opponentProfilePath(newName), { replace: true })}
                onGone={() => navigate('/opponents')}
              />
            )}
          </header>

          <UpcomingGames events={profile.upcomingEvents} />
          <VetoSuggestionPanel opponentName={profile.name} />
          <OpponentNotesSection opponentName={profile.name} notes={profile.notes} canManage={canManage} />
          <MapRecords maps={profile.maps} />
          <MatchHistory matches={profile.matches} />
        </>
      )}
    </div>
  )
}
