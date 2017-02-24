from app import app, socketio, db_web
from flask import render_template
from flask_socketio import emit
from requests import get
from urllib.parse import quote
import json

MESSAGE_HISTORY_SIZE = 100

@app.route('/play')
@app.route('/', subdomain='play')
def play_page():
    return render_template('play.html')

@socketio.on('connect')
def on_connect():
    print('User connected')

@socketio.on('startGame')
def handle_start(user_id):
    chat_history_clear(user_id)
    return emit('startGame', method('/start/web/%s' % user_id))

@socketio.on('sendMessage')
def handle_send_message(message):
    chat_history_add({"Message": {"From": message['id'], "Text": message['msg']}}, message['id'])
    return emit('sendMessage', method('/message/web/%s/%s' % (message['id'], message['msg'])))

@socketio.on('getMessages')
def handle_get_messages(user_id):
    method_result = method('/get/web/%s' % user_id)
    for message in json.loads(method_result):
        print(message)
        chat_history_add(message, user_id)

    return emit('getMessages', method_result)

@socketio.on('getMessageHistory')
def handle_get_chat_history(user_id):
    return emit('getMessages', chat_history_get(user_id))

@socketio.on('message')
def test(message):
    print(message)
    return

def method(path):
    '''HTTP-request'''
    request = get('http://localhost:9797' + quote(path))
    return request.text

def chat_history_add(text, user_id):
    user_data = db_web.find_one({'user_id': user_id})
    if not user_data:
        # create new user doc
        user_data = db_web.insert_one({'user_id': user_id, 'messages': [text]})

    else:
        # update user doc
        new_user_data = db_web.update_one(user_data,
                        {'$push': {'messages': {'$each': [text], '$slice': -MESSAGE_HISTORY_SIZE}}})

def chat_history_get(user_id):
    user_data = db_web.find_one({'user_id': user_id})
    if user_data:
        messages = user_data['messages']
        return json.dumps(messages)

def chat_history_clear(user_id):
    return db_web.delete_one({'user_id': user_id})