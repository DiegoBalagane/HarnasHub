/** Screen shown to a signed-in Guest whose access level hasn't been granted by a Manager yet. */
export function PendingAccessPage() {
  return (
    <>
      <h1 className="text-2xl font-semibold">Poczekaj na przydzielenie roli</h1>
      <p className="max-w-sm text-center text-sm text-neutral-400">
        Twoje konto zostało utworzone, ale manager drużyny musi jeszcze nadać Ci rolę — dopiero wtedy
        zobaczysz dane drużyny.
      </p>
    </>
  )
}
