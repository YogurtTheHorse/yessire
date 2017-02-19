from app import app, socketio

print('Starting Flask server...')
socketio.run(app, host='0.0.0.0', port=80)
