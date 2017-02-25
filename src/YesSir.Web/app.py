from flask import Flask
from flask_socketio import SocketIO
from pymongo import MongoClient

app = Flask(__name__)
app.config['SECRET_KEY'] = 'secret!'
app.config['SERVER_NAME'] = 'yessirgame.ru'
socketio = SocketIO(app)

mongo = MongoClient()
db_web = mongo['yes_sir_web']

from views import main_page
from views import play

if __name__ == '__main__':
    print('Starting Flask server...')
    socketio.run(app, port=80)
