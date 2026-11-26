/**
 * Primary endpoint: plain HTTP/WS on loopback. Browsers treat 127.0.0.1 as a
 * secure context, so HTTPS pages can call it without mixed-content errors.
 */
export const API_HOST = '127.0.0.1';
export const API_PORT = 41080;
export const API_BASE_URL = `http://${API_HOST}:${API_PORT}`;

/**
 * Deprecated HTTPS fallback for Safari, which still blocks http/ws requests to
 * loopback from HTTPS pages. Will be removed once Safari allows them, or by
 * November 2027 at the latest.
 */
export const FALLBACK_API_HOST = 'local.jmw.nz';
export const FALLBACK_API_PORT = 41443;
export const FALLBACK_API_BASE_URL = `https://${FALLBACK_API_HOST}:${FALLBACK_API_PORT}`;

/** Endpoints to try, in order. */
export const API_BASE_URLS = [API_BASE_URL, FALLBACK_API_BASE_URL];
