import matplotlib as mpl
mpl.use('Agg')

from flask import Flask, send_file
import matplotlib.pyplot as plt
from requests import get
from app import app
import json

from io import BytesIO

@app.route('/map')
def map():
    fig, ax = plt.subplots()
    fig.set_size_inches(18.5, 10.5)

    x = [ ]
    y = [ ]
    lables = [ ]

    for k in json.loads(get('http://localhost:9797/map').text):
        x.append(k['Position']['X'])
        y.append(k['Position']['Y'])

        xy = (k['Position']['X'], k['Position']['Y'])
        ax.annotate(k['Name'], xy=xy, xytext=(0, -20), textcoords='offset points', ha="center", bbox=dict(boxstyle='round,pad=0.5', fc='yellow', alpha=0.5))

    plt.plot(x, y, 'ro')
    plt.grid()
    img = BytesIO()
    plt.savefig(img, dpi=100)
    img.seek(0)
    return send_file(img, mimetype='image/png')
