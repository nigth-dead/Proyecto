exec { 'install_docker_repository':
  command => '/usr/bin/install -m 0755 -d /etc/apt/keyrings &&
              /usr/bin/curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc &&
              /usr/bin/chmod a+r /etc/apt/keyrings/docker.asc &&
              /bin/sh -c "echo deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo $VERSION_CODENAME) stable > /etc/apt/sources.list.d/docker.list" &&
              /usr/bin/apt-get update',
  creates => '/etc/apt/sources.list.d/docker.list',
}

package { [
  'docker-ce',
  'docker-ce-cli',
  'containerd.io',
  'docker-buildx-plugin',
  'docker-compose-plugin'
]:
  ensure  => installed,
  require => Exec['install_docker_repository'],
}

service { 'docker':
  ensure  => running,
  enable  => true,
  require => Package['docker-ce'],
}

exec { 'add_vagrant_user_to_docker_group':
  command => '/usr/sbin/usermod -aG docker vagrant',
  unless  => '/usr/bin/id -nG vagrant | /usr/bin/grep -qw docker',
  require => Package['docker-ce'],
}

exec { 'deploy_containers':
  command => '/usr/bin/docker compose up -d --build',
  cwd     => '/vagrant',
  require => Service['docker'],
}