FROM python:3.8
WORKDIR /app
COPY . .
EXPOSE 80
RUN python -m pip install plac colorama requests pyjwt
RUN chmod +x routes/*
CMD ["python", "main.py"]