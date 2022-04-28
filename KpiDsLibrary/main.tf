provider "aws" {
  region = "eu-central-1"
}

data "aws_ami" "ubuntu" {
  most_recent = true

  filter {
    name   = "name"
    values = ["ubuntu/images/hvm-ssd/ubuntu-focal-20.04-amd64-server-*"]
  }

  filter {
    name   = "virtualization-type"
    values = ["hvm"]
  }

  owners = ["099720109477"] 
}

resource "aws_eip" "lb" {
  instance = aws_instance.docker_site.id
  vpc      = true
}

resource "aws_instance" "docker_site" {
  ami                         = data.aws_ami.ubuntu.id
  instance_type               = "t2.micro"
  vpc_security_group_ids      = [aws_security_group.allow_web.id]
  key_name                    = "connect_key"
  iam_instance_profile = aws_iam_instance_profile.log_profile.name

    connection {
      type        = "ssh"
      host        = self.public_ip
      user        = "ubuntu"
      private_key = file("connect_key.pem")
}

    provisioner "remote-exec" {
    inline = [
    "sudo apt-get update && install curl",
    "curl -sSL https://get.docker.com/ | sh",
    "sudo docker run -d --name docker_site --log-driver=awslogs --log-opt awslogs-group=logs -p 80:80 straxseller/devops_prac",
    "sudo docker run -d --name watchtower -v /var/run/docker.sock:/var/run/docker.sock containrrr/watchtower --cleanup -i 10",
    ]
  }

  tags = {
    Name = "docker_site"
  }
}

resource "aws_security_group" "allow_web" {
  name        = "allow_web_traffic"
  description = "Allow Web inbound traffic"

  ingress {
    description = "HTTP"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description = "SSH"
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

output "Elastic_IP" {
  value = aws_instance.docker_site.public_ip
}
