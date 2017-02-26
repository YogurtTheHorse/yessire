from flask import Flask
from flask_socketio import SocketIO
from pymongo import MongoClient
from flask.ext.login import LoginManager

app = Flask(__name__)
app.config['SECRET_KEY'] = 'secret!'
app.config['SERVER_NAME'] = 'yessirgame.ru'

socketio = SocketIO(app)

lm = LoginManager()
lm.init_app(app)

mongo = MongoClient()
db_web = mongo['yes_sir_web']['messages']
app.config['USERS_COLLECTION'] = mongo['yes_sir_web']['users']

from views import main_page
from views import play
from views import login
from views import map

if __name__ == '__main__':
    print('Starting Flask server...')
    socketio.run(app, port=80)
