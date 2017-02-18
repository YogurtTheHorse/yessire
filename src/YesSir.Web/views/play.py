from app import app, socketio
from flask import render_template
from flask_socketio import emit
from urllib.request import urlopen

@app.route('/play')
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

def method(path):
    '''HTTP-request'''
    print('http://localhost:9797' + path)
    return urlopen('http://localhost:9797' + path).read()