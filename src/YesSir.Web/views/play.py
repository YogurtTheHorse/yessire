from app import app, socketio
from flask import render_template
from urllib.request import urlopen

@app.route('/play')
def play_page():
    return render_template('play.html')

@socketio.on('connect')
def connect():
    print('User connected')

@socketio.on('startGame')
def handle_start(id):
    return method('/start/web/%s' % id)

@socketio.on('message')
def handle_message(data):
    return method('/message/web/%s/%s' % (data.id, data.msg))

@socketio.on('getMessages')
def handle_get(id):
    return method('/get/web/%s' % id)

def method(path):
    '''HTTP-request'''
    print(path)
    return urlopen('http://localhost:9797' + path).read()