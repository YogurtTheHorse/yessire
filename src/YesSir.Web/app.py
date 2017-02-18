from flask import Flask
from flask_socketio import SocketIO

app = Flask(__name__)
app.config['SECRET_KEY'] = 'secret!'
socketio = SocketIO(app)

from views import main_page
from views import play

if __name__ == '__main__':
    print('Starting Flask server...')
    socketio.run(app)

@socketio.on('message')
def handle_message(message):
    print('received message: ' + message)