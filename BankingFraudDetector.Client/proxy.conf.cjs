// Local development only; never disable certificate checks for production calls.
module.exports = {
  '/api/**': {
    target: process.env.API_TARGET || 'https://127.0.0.1:7064',
    secure: false,
    changeOrigin: true,
    timeout: 600000,
    proxyTimeout: 600000
  }
};
