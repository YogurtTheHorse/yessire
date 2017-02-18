from app import app, socketio

print('Starting Flask server...')
socketio.run(app)