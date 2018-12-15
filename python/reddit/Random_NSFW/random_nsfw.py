import random
import re

import praw
import prawcore

with open("config", "rt") as f:
	conf = f.readlines()
	idc = conf[0]
	if "\n" in idc:
		idc = idc.split('\n')[0]
	sect = conf[1]

reddit = praw.Reddit(client_id=idc,
					 client_secret=sect,
					 user_agent='SansBot')

subreddit = reddit.random_subreddit(nsfw=True)

post = subreddit.random()

def get_post():
	post = subreddit.random()
	return post

post = get_post()

i = 0

while (".jpg" not in post.url and i < 10):
	post = get_post()
	i += 1

url = post.url


with open("URL.sans", "wt") as f:
	f.write(url)
