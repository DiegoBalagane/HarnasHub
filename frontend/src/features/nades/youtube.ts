/** Extracts a YouTube video id from a watch/share/embed URL, or null if the URL isn't recognized. */
export function getYoutubeEmbedUrl(url: string): string | null {
  const match = url.match(/(?:youtube\.com\/watch\?v=|youtu\.be\/|youtube\.com\/embed\/)([\w-]{11})/)
  return match ? `https://www.youtube.com/embed/${match[1]}` : null
}
