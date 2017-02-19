from app import app, socketio
from flask import render_template
from flask_socketio import emit
from requests import get
from urllib.parse import quote

@app.route('/play')
@app.route('/', subdomain='play')
def play_page():
    return render_template('play.html')

@socketio.on('connect')
def on_connect():
    print('User connected')

@socketio.on('startGame')
def handle_start(message):
    return emit('startGame', method('/start/web/%s' % message))

@socketio.on('sendMessage')
def handle_send_message(message):
    print(message)
    return emit('sendMessage', method('/message/web/%s/%s' % (message['id'], message['msg'])))

@socketio.on('getMessages')
def handle_get_messages(message):
    return emit('getMessages', method('/get/web/%s' % message))

@socketio.on('message')
def test(message):
    print(message)
    return

def method(path):
    '''HTTP-request'''
    request = get('http://localhost:9797' + quote(path))
    return request.text