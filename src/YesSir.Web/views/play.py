from app import app, socketio
from flask import render_template
from flask_socketio import send
from urllib.request import urlopen

@app.route('/play')
def play_page():
    return render_template('play.html')

@socketio.on('connect')
def connect():
    print('User connected')

@socketio.on('startGame')
def handle_start(id):
    return send(method('/start/web/%s' % id))

@socketio.on('message')
def handle_message(json):
    return send(method('/message/web/%s/%s' % (json.id, json.msg)))

@socketio.on('getMessages')
def handle_get(id):
    return send(method('/get/web/%s' % id))

def method(path):
    '''HTTP-request'''
    return urlopen('http://localhost:9797' + path).read()