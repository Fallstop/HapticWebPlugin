import { json } from '@sveltejs/kit';
import type { RequestHandler } from './$types';

export const GET: RequestHandler = async () => {
	try {
		const res = await fetch('https://marketplace.logi.com/api/downloads', {
			signal: AbortSignal.timeout(5000)
		});
		if (!res.ok) return json({ downloads: null });
		const data = await res.json();
		const count = data?.plugins?.HapticWeb?.downloads;
		if (count != null) {
			return json({ downloads: count });
		}
	} catch {
		/* ignore */
	}
	return json({ downloads: null });
};
