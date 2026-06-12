// Dev-server proxy: forwards /api to the backend.
// Override with API_PROXY_TARGET (Docker .env.docker or shell env).
const target = process.env.API_PROXY_TARGET || 'http://localhost:5031';

module.exports = {
  '/api': {
    target,
    secure: false,
    changeOrigin: true
  }
};
